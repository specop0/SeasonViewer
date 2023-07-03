package anime.seleniumminer.main;

import anime.seleniumminer.plugin.IPlugin;
import anime.seleniumminer.plugin.Plugins;
import anime.seleniumminer.plugin.webdriver.FirefoxDriverEx;
import anime.seleniumminer.plugin.webdriver.IWebDriver;
import anime.seleniumminer.routes.Routes;
import java.io.IOException;

public class Main {

    public static void main(String[] args) throws IOException {

        try {

            int port = 5022;

            IWebDriver driver = FirefoxDriverEx.GetNewWebDriver();
            IPlugin[] plugins = Plugins.GetAllPlugins();
            IMineController controller = new MineController(driver, plugins);

            Routes.EstablishRoutes(port, controller);

            System.out.println(String.format("Endpoint listening at: localhost:%d", port));

            System.in.read();
            spark.Spark.stop();
            spark.Spark.awaitStop();
        } catch (Throwable  ex) {
            System.out.println(ex.getMessage());
            System.out.println(ex);
        }
    }
}
