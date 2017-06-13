package storage;

// An interface to handle inter-invocation persistence (eg. the player ID
// is generated and stored persistently).
interface IStorage
{
    public function has(key:String):Bool;
    public function get(key:String):Any;
    public function set(key:String, value:Any):Void;
}