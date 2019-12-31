package anime.seleniumminer.main;

import anime.seleniumminer.plugin.IPlugin;
import anime.seleniumminer.plugin.Plugins;
import anime.seleniumminer.plugin.webdriver.ChromeDriverEx;
import anime.seleniumminer.plugin.webdriver.IWebDriver;
import anime.seleniumminer.routes.Routes;
import java.io.IOException;

public class Main {

    public static void main(String[] args) throws IOException {

        int port = 22471;
        String settingsFilename = "settings.json";
        if (args.length > 0) {
            settingsFilename = args[0];
        }

        IWebDriver driver = ChromeDriverEx.GetNewWebDriver();
        IPlugin[] plugins = Plugins.GetAllPlugins();
        IMineController controller = new MineController(driver, plugins);

        Routes.EstablishRoutes(port, controller);

        System.out.println(String.format("Endpoint listening at: localhost:%d", port));

        System.in.read();
        spark.Spark.stop();
        spark.Spark.awaitStop();
    }
}
