package anime.seleniumminer.main;

import anime.seleniumminer.plugin.IPlugin;
import anime.seleniumminer.plugin.webdriver.IWebDriver;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Function;
import org.json.JSONObject;

public class MineController implements IMineController {

    private final IWebDriver Driver;
    private final Map<String, IPlugin> Plugins;

    public MineController(IWebDriver driver, IPlugin[] plugins) {
        this.Driver = driver;
        this.Plugins = new HashMap<>();
        for (IPlugin plugin : plugins) {
            this.Plugins.put(plugin.GetPluginId(), plugin);
        }
    }

    @Override
    public synchronized JSONObject RequestDriver(Function<IWebDriver, JSONObject> function) {
        JSONObject result = null;

        try {
            result = function.apply(this.Driver);
        } catch (Exception ex) {

        }
        this.Driver.loadPage("about:blank");

        return result;
    }

    @Override
    public IPlugin GetPlugin(String pluginId) {
        String pluginIdInvariant = pluginId.toLowerCase();
        if (this.Plugins.containsKey(pluginIdInvariant)) {
            return this.Plugins.get(pluginIdInvariant);
        }

        return null;
    }
}
