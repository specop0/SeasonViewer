package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import org.json.JSONObject;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;

public class DuckDuckGoPlugin implements IPlugin {

    @Override
    public String GetPluginId() {
        return "duckduckgo";
    }

    @Override
    public JSONObject HandleRequest(IWebDriver driver, JSONObject data) {
        driver.loadPage("https://duckduckgo.com");

        WebElement searchInputField = driver.findElement(By.id("search_form_input_homepage"));

        String search = data.getString("search");
        searchInputField.sendKeys(search);
        searchInputField.submit();

        // search element again to wait for page load
        driver.findElement(By.className("results"));
        try {
            driver.findElement(By.id("r1-0"));
        } catch (Exception ex) {
            // handled
        }
        String pageSource = driver.getPageSourceJS();

        JSONObject result = new JSONObject();
        result.put("pageSource", pageSource);
        return result;
    }
}
