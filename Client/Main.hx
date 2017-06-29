import haxe.io.Bytes;
import thx.http.RequestInfo;
import thx.http.RequestType;
using thx.http.Request;
using thx.Arrays;
using thx.Strings;
#if thx_stream
using thx.stream.Stream;
#end

import protonanalytics.AnalyticsClient;

@:expose
@:keep
class Main extends openfl.display.Sprite
{
    private static inline var API_KEY:String = "NLPrclNm1ybTnwcBrwpzp/KcSsn8go8b4tjdRL90Lw8=";

	public function new()
	{
        super();
        var client = new AnalyticsClient(API_KEY);
        client.startSession();
	}
}