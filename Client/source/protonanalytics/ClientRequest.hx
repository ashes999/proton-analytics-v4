package protonanalytics;

class ClientRequest
{
    public var httpVerb(default, null):String;
    public var url(default, null):String;
    public var body(default, null):String;

    public function new(httpVerb:String, url:String, body:String)
    {
        this.httpVerb = httpVerb.toUpperCase();
        this.url = url;
        this.body = body;
    }
}