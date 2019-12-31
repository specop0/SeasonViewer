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
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeDriverService;
import org.openqa.selenium.chrome.ChromeOptions;
import org.openqa.selenium.remote.UnreachableBrowserException;

public class ChromeDriverEx extends ChromeDriver implements IWebDriver {

    private static final String chromeDriverDefaultPath = "chromedriver.exe";

    public ChromeDriverEx(ChromeDriverService chromeService, ChromeOptions chromeOptions) {
        super(chromeService, chromeOptions);
        int timeout = 30;
        manage().timeouts().pageLoadTimeout(timeout, TimeUnit.SECONDS);
        manage().timeouts().setScriptTimeout(timeout, TimeUnit.SECONDS);
        manage().timeouts().implicitlyWait(timeout, TimeUnit.SECONDS);
    }

    public static IWebDriver GetNewWebDriver() {
        return GetNewWebDriver(chromeDriverDefaultPath);
    }

    public static IWebDriver GetNewWebDriver(String chromeDriverPath) {
        IWebDriver webDriver = null;
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.setBinary("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe");

        chromeOptions.addArguments("start-maximized"); // https://stackoverflow.com/a/26283818/1689770
        chromeOptions.addArguments("enable-automation"); // https://stackoverflow.com/a/43840128/1689770
        //chromeOptions.addArguments("--headless"); // only if you are ACTUALLY running headless
        chromeOptions.addArguments("--no-sandbox"); //https://stackoverflow.com/a/50725918/1689770
        chromeOptions.addArguments("--disable-infobars"); //https://stackoverflow.com/a/43840128/1689770
        chromeOptions.addArguments("--disable-dev-shm-usage"); //https://stackoverflow.com/a/50725918/1689770
        chromeOptions.addArguments("--disable-browser-side-navigation"); //https://stackoverflow.com/a/49123152/1689770
        chromeOptions.addArguments("--disable-gpu"); //https://stackoverflow.com/questions/51959986/how-to-solve-selenium-chromedriver-timed-out-receiving-message-from-renderer-exc

        chromeOptions.addArguments("--dns-prefetch-disable");

        chromeOptions.setPageLoadStrategy(PageLoadStrategy.NONE);

        chromeOptions.addArguments("--disable-bundled-ppapi-flash");
        /*
        * 1. Locate extensions folder under
        * C:\Users\SpecOp0\AppData\Local\Google\Chrome\User Data\Default\Extensions
        * Ghostery: mlomiejdfkolichcflejclcbmpeaniij
        * uBlock Origin: cjpalhdlnbpafiamejdnhcphjbkeiagm
        * 2. chrome://extensions/
        * 3. Activate Developer Mode (top right)
        * 4. Pack Extension from path 
         */
        String[] extensions = {
            "uBlock-Origin_v1.15.24.crx", //"Ghostery_v8.1.0.crx"
        };
        for (String extensionFilename : extensions) {
            File extensionFile = new File(extensionFilename);
            chromeOptions.addExtensions(extensionFile);
        }
        File chromeDriver = new File("chromedriver.exe");
        ChromeDriverService chromeService = new ChromeDriverService.Builder().usingDriverExecutable(chromeDriver).usingAnyFreePort().build();
        webDriver = new ChromeDriverEx(chromeService, chromeOptions);
        return webDriver;
    }

// <editor-fold desc="Load page handling">
    @Override
    public void loadPage(String url) {
        try {
            this.get(url);

            if ("about:blank".equals(url)) {
                return;
            }

            // PageLoadStrategy=None, so wait some seconds
            try {
                Thread.sleep(5000);

            } catch (InterruptedException ex) {
                Logger.getLogger(ChromeDriver.class
                        .getName()).log(Level.SEVERE, null, ex);
            }
        } catch (TimeoutException ex) {
            webDriverStopLoading();
        } catch (UnhandledAlertException ex) {
            webDriverHandleAlert();
        } catch (UnreachableBrowserException | NoSuchWindowException ex) {
            throw ex;
        } catch (WebDriverException ex) {
            throw ex;
        }
    }

    public void webDriverStopLoading() {
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
        } catch (WebDriverException ex) {
            String message = "WebDriver exception: " + ex.getMessage();
        }
    }

    public void webDriverHandleAlert() {
        try {
            Alert alert = this.switchTo().alert();
            alert.accept();
        } catch (NoAlertPresentException noAlertEx) {
            // handled
        }
    }
    // </editor-fold>

    // <editor-fold desc="Click routine">
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
            return false;
        }

        return true;
    }

    // </editor-fold>
    @Override
    public String getPageSourceJS() {
        String javascript = "return document.getElementsByTagName('html')[0].innerHTML";
        String pageSource = (String) ((JavascriptExecutor) this).executeScript(javascript);
        pageSource = "<html>" + pageSource + "</html>";
        return pageSource;
    }
}
