package anime.seleniumminer.main;

import anime.seleniumminer.plugin.IPlugin;
import anime.seleniumminer.plugin.webdriver.IWebDriver;
import java.util.function.Function;
import org.json.JSONObject;

public interface IMineController {
    JSONObject RequestDriver(Function<IWebDriver, JSONObject> action);
    
    IPlugin GetPlugin(String pluginId);
}
