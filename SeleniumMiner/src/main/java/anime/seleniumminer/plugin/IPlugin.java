package anime.seleniumminer.plugin;

import anime.seleniumminer.plugin.webdriver.IWebDriver;
import org.json.JSONObject;

public interface IPlugin {
    
    String GetPluginId();
    
    JSONObject HandleRequest(IWebDriver driver, JSONObject data);
}
