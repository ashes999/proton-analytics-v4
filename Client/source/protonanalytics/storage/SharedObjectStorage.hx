package protonanalytics.storage;

import openfl.net.SharedObject;

// Uses OpenFL's SharedObject class to persist data
class SharedObjectStorage implements IStorage
{
    private static inline var SHARED_OBJECT_STORAGE_NAME = "Proton Analytics client data";
    private var storage:SharedObject;

    public function new()
    {
        this.storage = SharedObject.getLocal(SHARED_OBJECT_STORAGE_NAME);
    }

    public function has(key:String):Bool
    {
        // https://github.com/openfl/openfl/issues/1596#issuecomment-307449966
        var obj:haxe.DynamicAccess<Dynamic> = this.storage.data;
        return obj.exists(key);
    }

    public function get(key:String):Any
    {
        var obj:haxe.DynamicAccess<Dynamic> = this.storage.data;
        return obj.get(key);
    }

    public function set(key:String, value:Any):Void
    {
        var obj:haxe.DynamicAccess<Dynamic> = this.storage.data;
        obj.set(key, value);
        this.storage.flush();
    }
}