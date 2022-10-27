package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import org.json.JSONObject;
import org.openqa.selenium.By;
import org.openqa.selenium.OutputType;
import org.openqa.selenium.WebElement;

public class ScreenshotPlugin implements IPlugin {

    @Override
    public String GetPluginId() {
        return "screenshot";
    }

    @Override
    public JSONObject HandleRequest(IWebDriver driver, JSONObject data) {
        String requestUrl = data.getString("url");

        driver.loadPage(requestUrl);

        WebElement imageElement = driver.findElement(By.tagName("img"));
        String encodedScreenshot = imageElement.getScreenshotAs(OutputType.BASE64);

        JSONObject result = new JSONObject();
        result.put("imageData", encodedScreenshot);
        return result;
    }

}
