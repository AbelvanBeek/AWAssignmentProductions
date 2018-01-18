void BitSet( uint x, uint y, __global uint* arr, uint w ) { arr[y * 54 + (x >> 5)] |= 1U << (uint)(x & 31); }

uint GetBit( uint x, uint y, __global uint* arr, uint w ) { return (arr[y * w + (x >> 5)] >> (int)(x & 31)) & 1U; }

void Simulate(uint x, uint y, __global uint* patt, __global uint* sec)
	{		
		// process all pixels, skipping one pixel boundary
		uint neww = 54;
			uint n = GetBit( x - 1, y - 1, sec, neww ) + GetBit( x, y - 1, sec, neww ) + GetBit( x + 1, y - 1, sec, neww ) + GetBit( x - 1, y, sec, neww ) + 
					 GetBit( x + 1, y, sec, neww ) + GetBit( x - 1, y + 1, sec, neww ) + GetBit( x, y + 1, sec, neww ) + GetBit( x + 1, y + 1, sec, neww );
			if (GetBit( x, y, sec, neww) == 1)
			{
			if(n == 2)
				{
				BitSet( x, y, patt, neww );
				}
			}
			if(n == 3)
				{
				BitSet( x, y, patt, neww );
				}
	}

__kernel void device_function( 
	write_only image2d_t a,
		uint width, uint height,
		__global uint* pattern,
		__global uint* second )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;
	if (id >= (512 * 512)) return;
	Simulate(idx, idy, pattern, second);
	int2 pos = (int2)( idx, idy );
	write_imagef( a, pos, GetBit(idx, idy, second, width));
}

__kernel void clear_function(
	write_only image2d_t a)
{
	// get the thread id; this will be our pixel id
	int x = get_global_id( 0 );
	int y = get_global_id( 1 );
	int id = x + 512 * y;
	if (id >= (512 * 512)) return;
	write_imagef( a, (int2)(x, y), 1);
}