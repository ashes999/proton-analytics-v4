package storage;

interface IStorage
{
    public function has(key:String):Bool;
    public function get(key:String):Any;
    public function set(key:String, value:Any):Void;
}