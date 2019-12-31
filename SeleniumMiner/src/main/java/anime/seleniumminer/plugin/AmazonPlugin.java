package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import java.util.logging.Level;
import java.util.logging.Logger;
import org.json.JSONObject;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;

public class AmazonPlugin implements IPlugin {

    @Override
    public String GetPluginId() {
        return "amazon";
    }

    @Override
    public JSONObject HandleRequest(IWebDriver driver, JSONObject data) {
        String requestUrl = data.getString("url");
        driver.loadPage(requestUrl);
        
        WebElement navSearchField = driver.findElement(By.className("nav-search-field"));
        WebElement searchInputField = navSearchField.findElement(By.tagName("input"));
        
        String search = data.getString("search");
        searchInputField.sendKeys(search);
        searchInputField.submit();
        
        // search element again to wait for page load
        navSearchField = driver.findElement(By.className("nav-search-field"));
        try {
            Thread.sleep(1500);
        } catch (InterruptedException ex) {
            Logger.getLogger(AmazonPlugin.class.getName()).log(Level.SEVERE, null, ex);
        }
        String pageSource = driver.getPageSourceJS();

        JSONObject result = new JSONObject();
        result.put("pageSource", pageSource);
        return result;
    }

}
