package anime.seleniumminer.routes;

import anime.seleniumminer.main.IMineController;

public class Routes {

    public static void EstablishRoutes(int port, IMineController controller) {
        spark.Spark.port(port);
        spark.Spark.post("/mine/*", "application/json", new MineRoute(controller));
    }
}
