namespace Kayateia.Climoo
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

static public class ShaHasher
{
	static public string Sha1Hash( this string input )
	{
		byte[] key = System.Text.Encoding.UTF8.GetBytes( input );
		SHA1 sha1 = SHA1Managed.Create();
		byte[] hash = sha1.ComputeHash(key);
		string rv = "";
		foreach( byte b in hash )
			rv += "{0:x2}".FormatI( b );
		return rv;
	}
}

}
