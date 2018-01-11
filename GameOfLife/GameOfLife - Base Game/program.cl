void BitSet( uint x, uint y, global uint * arr, uint w ) { arr[y * w + (x >> 5)] |= 1U << (uint)(x & 31); }
uint GetBit( uint x, uint y, global uint * arr, uint w ) { return (arr[y * w + (x >> 5)] >> (int)(x & 31)) & 1U; }

void Simulate(uint w, uint h, global uint * patt, global uint * sec)
	{		
		// process all pixels, skipping one pixel boundary
		uint neww = w * 32, newh = h;
		for( uint y = 1; y < newh - 1; y++ ) for( uint x = 1; x < neww - 1; x++ )
		{
			// count active neighbors
			uint n = GetBit( x - 1, y - 1, sec, neww ) + GetBit( x, y - 1, sec, neww ) + GetBit( x + 1, y - 1, sec, neww ) + GetBit( x - 1, y, sec, neww ) + 
					 GetBit( x + 1, y, sec, neww ) + GetBit( x - 1, y + 1, sec, neww ) + GetBit( x, y + 1, sec, neww ) + GetBit( x + 1, y + 1, sec, neww );
			if ((GetBit( x, y, sec, neww ) == 1 && n ==2) || n == 3) BitSet( x, y, patt, neww );
		}

	}

__kernel void device_function( 
	write_only image2d_t a,
		uint width, uint height,
		__global uint * pattern,
		__global uint * second )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;
	if (id >= (512 * 512)) return;
	//Simulate(idx, idy, pattern, second);
	int2 pos = (int2)( idx, idy );
	write_imagef( a, pos, pattern[idx * idy] );
}