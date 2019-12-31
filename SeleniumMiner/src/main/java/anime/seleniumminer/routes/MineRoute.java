package anime.seleniumminer.routes;

import anime.seleniumminer.main.IMineController;
import anime.seleniumminer.plugin.IPlugin;
import java.net.HttpURLConnection;
import org.json.JSONObject;
import spark.Request;
import spark.Response;
import spark.Route;

public class MineRoute implements Route {

    protected final IMineController Controller;

    public MineRoute(IMineController controller) {
        this.Controller = controller;
    }

    @Override
    public Object handle(Request request, Response response) throws Exception {
        String[] urlWildcards = request.splat();
        if (urlWildcards.length != 1) {
            response.status(HttpURLConnection.HTTP_BAD_REQUEST);
            return null;
        }

        String pluginId = urlWildcards[0];
        IPlugin plugin = this.Controller.GetPlugin(pluginId);
        if (plugin == null) {
            response.status(HttpURLConnection.HTTP_BAD_REQUEST);
            return null;
        }

        // input must be JSON
        JSONObject data = new JSONObject();
        String dataRaw = request.body();
        if (dataRaw != null && !"".equals(dataRaw)) {
            data = new JSONObject(dataRaw);
        }
        final JSONObject actualData = data;

        JSONObject json = this.Controller.RequestDriver(driver -> plugin.HandleRequest(driver, actualData));
        
        response.type("application/json");
        return json;
    }

}
