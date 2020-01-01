package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import org.json.JSONObject;
import org.openqa.selenium.By;

public class PageSourcePlugin implements IPlugin {

    @Override
    public String GetPluginId() {
        return "pagesource";
    }

    @Override
    public JSONObject HandleRequest(IWebDriver driver, JSONObject data) {
        String requestUrl = data.getString("url");

        driver.loadPage(requestUrl);

        // some pages load too many data, so request some elements to be safe
        if (requestUrl.startsWith("https://myanimelist.net/animelist")) {
            driver.findElements(By.className("animetitle"));
        }

        String pageSource = driver.getPageSourceJS();

        JSONObject result = new JSONObject();
        result.put("pageSource", pageSource);
        return result;
    }

}
