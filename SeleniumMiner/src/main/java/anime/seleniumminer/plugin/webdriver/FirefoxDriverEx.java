package anime.seleniumminer.plugin.webdriver;

import java.io.File;
import java.util.concurrent.TimeUnit;
import java.util.logging.Level;
import java.util.logging.Logger;

import org.openqa.selenium.Alert;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.Keys;
import org.openqa.selenium.NoAlertPresentException;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.NoSuchWindowException;
import org.openqa.selenium.PageLoadStrategy;
import org.openqa.selenium.TimeoutException;
import org.openqa.selenium.UnhandledAlertException;
import org.openqa.selenium.WebDriverException;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxOptions;
import org.openqa.selenium.firefox.GeckoDriverService;
import org.openqa.selenium.remote.CapabilityType;
import org.openqa.selenium.remote.UnreachableBrowserException;

public class FirefoxDriverEx extends FirefoxDriver implements IWebDriver {

    public FirefoxDriverEx(GeckoDriverService service, FirefoxOptions options) {
        super(service, options);
        int timeout = 30;
        manage().timeouts().pageLoadTimeout(timeout, TimeUnit.SECONDS);
        manage().timeouts().setScriptTimeout(timeout, TimeUnit.SECONDS);
        manage().timeouts().implicitlyWait(timeout, TimeUnit.SECONDS);

        Object loadStrategyRaw = options.getCapability(CapabilityType.PAGE_LOAD_STRATEGY);
        if (loadStrategyRaw == null){
            this.LoadStrategy = PageLoadStrategy.NORMAL;
        } else {
            this.LoadStrategy = (PageLoadStrategy)loadStrategyRaw;
        }
    }

    protected final PageLoadStrategy LoadStrategy;

    public static IWebDriver GetNewWebDriver() {
        IWebDriver webDriver = null;
        FirefoxOptions options = new FirefoxOptions();

        // options.setPageLoadStrategy(PageLoadStrategy.NONE);

        File driver = new File("geckodriver");

        GeckoDriverService chromeService = new GeckoDriverService
            .Builder()
            .usingDriverExecutable(driver)
            .usingAnyFreePort()
            .build();
        webDriver = new FirefoxDriverEx(chromeService, options);
        return webDriver;
    }

    @Override
    public void loadPage(String url) {
        try {
            this.get(url);

            if ("about:blank".equals(url)) {
                return;
            }

            // PageLoadStrategy=None, so wait some seconds
            if (this.LoadStrategy == PageLoadStrategy.NONE) {
                this.waitSomeSeconds(5);
            }
        } catch (TimeoutException ex) {
            stopLoading();
        } catch (UnhandledAlertException ex) {
            handleAlert();
        } catch (UnreachableBrowserException | NoSuchWindowException ex) {
            throw ex;
        } catch (WebDriverException ex) {
            throw ex;
        }
    }

    public void stopLoading() {
        try {
            try {
                this.findElement(By.id("body")).sendKeys(Keys.ESCAPE);
                this.findElement(By.id("video")).sendKeys(Keys.SPACE);
            } catch (NoSuchElementException ex2) {
                // handled
            }
            ((JavascriptExecutor) this).executeScript("document.getElementsByTagName(\"video\")[0].pause()");
            ((JavascriptExecutor) this).executeScript("window.stop()");
        } catch (TimeoutException ex) {
            String message = "Timeout at webDriver stop: " + ex.getMessage();
            Logger.getLogger(FirefoxDriverEx.class.getName()).log(Level.SEVERE, message, ex);
        } catch (WebDriverException ex) {
            String message = "WebDriver exception: " + ex.getMessage();
            Logger.getLogger(FirefoxDriverEx.class.getName()).log(Level.SEVERE, message, ex);
        }
    }

    public void handleAlert() {
        try {
            Alert alert = this.switchTo().alert();
            alert.accept();
        } catch (NoAlertPresentException noAlertEx) {
            // handled
        }
    }

    protected void closeAllPopupWindows(String mainWindowHandle) {
        for (String windowHandle : this.getWindowHandles()) {
            if (!windowHandle.equals(mainWindowHandle)) {
                this.switchTo().window(windowHandle);
                this.close();
            }
        }
        this.switchTo().window(mainWindowHandle);
    }

    public void waitSomeSeconds(long seconds) {
        try {
            Thread.sleep(seconds * 1000);
        } catch (InterruptedException ex) {
        }
    }

    @Override
    public boolean click(WebElement elementToClick) {
        String mainHandle = this.getWindowHandle();
        return clickInternal(elementToClick, mainHandle);
    }

    private boolean clickInternal(WebElement elementToClick, String mainHandle) {
        try {
            if (!elementToClick.isDisplayed()) {
                JavascriptExecutor js = (JavascriptExecutor) this;
                js.executeScript("arguments[0].click();", elementToClick);
            } else {
                elementToClick.click();
            }
            waitSomeSeconds(1);
            closeAllPopupWindows(mainHandle);
        } catch (WebDriverException ex) {
            String message = String.format(
                    "Error while clicking element (e.g. handling ad): %s;%s;%s",
                    elementToClick.toString(),
                    elementToClick.getAttribute("tag"),
                    elementToClick.getAttribute("class"));
            Logger.getLogger(FirefoxDriverEx.class.getName()).log(Level.SEVERE, message, ex);
            return false;
        }

        return true;
    }

    @Override
    public String getPageSourceJS() {
        String javascript = "return document.getElementsByTagName('html')[0].innerHTML";
        String pageSource = (String) ((JavascriptExecutor) this).executeScript(javascript);
        pageSource = "<html>" + pageSource + "</html>";
        return pageSource;
    }
}
