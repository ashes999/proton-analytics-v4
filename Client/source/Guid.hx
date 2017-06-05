// Based on https://gist.github.com/ciscoheat/4b1797fa56648adac163f44186f1823a
// Any changes include the previous version as commented-out code
// TODO: move into Noor
class Guid
{
	public static function newGuid():String
    {
		// Based on https://gist.github.com/LeverOne/1308368
		var uid = new StringBuf();
        
		var a = 8;
		
		// Original code: uid.add(StringTools.hex(Std.int(Date.now().getTime()), 8));
		// The original algorithm generates GUIDs that always start with 80000000.
		// Replace the first eight digits with a random hex number. (Max that fits is 2^32.)
		
		// Another complication: Neko ints are 31 bits, everything else is 32 bits
		#if neko
		var numBits = 31;
		#else
		var numBits = 32;
		#end

		var aNumber = Std.int(Math.pow(2, numBits) * Math.random());
		uid.add(StringTools.hex(aNumber, 8));

		while((a++) < 36)
        {
			uid.add(a*51 & 52 != 0
				? StringTools.hex(a^15 != 0 ? 8^Std.int(Math.random() * (a^20 != 0 ? 16 : 4)) : 4)
				: "-"
			);
		}
		return uid.toString().toLowerCase();
	}
}