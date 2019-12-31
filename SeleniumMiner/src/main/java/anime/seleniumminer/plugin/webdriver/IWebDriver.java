package anime.seleniumminer.plugin.webdriver;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public interface IWebDriver extends WebDriver {

    public static By ChildNodes = By.xpath("./*");
    public static By ParentElement = By.xpath("..");
    
    public void loadPage(String url);

    public static WebElement parentElement(WebElement child) {
        return child.findElement(ParentElement);
    }
    
    public boolean click(WebElement elementToClick);

    public String getPageSourceJS();
}