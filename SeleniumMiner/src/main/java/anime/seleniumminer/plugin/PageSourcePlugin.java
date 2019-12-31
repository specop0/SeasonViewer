package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import org.json.JSONObject;

public class PageSourcePlugin implements IPlugin {

    @Override
    public String GetPluginId() {
        return "pagesource";
    }

    @Override
    public JSONObject HandleRequest(IWebDriver driver, JSONObject data) {
        String requestUrl = data.getString("url");

        driver.loadPage(requestUrl);
        String pageSource = driver.getPageSourceJS();

        JSONObject result = new JSONObject();
        result.put("pageSource", pageSource);
        return result;
    }

}
