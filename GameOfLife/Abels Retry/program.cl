// helper function for setting one bit in the pattern buffer
void BitSet( uint x, uint y, uint pw, global uint *pattern ) { pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31); }

void TestBitSet(uint x, uint y, uint pw, global uint *pattern)
{
	if (pattern[x + pw * y] == 4294967295)
		pattern[x + pw * y] = 0;
	else
		pattern[x + pw * y] = 4294967295;
}

// helper function for getting one bit from the secondary pattern buffer
uint GetBit( uint x, uint y, uint pw, global uint *second ) { return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; }

__kernel void simulate_function( global uint *second, global uint *pattern, uint pw, uint ph )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + ph * idy;
	//if (id >= (pw * 32 * ph)) return;

	int x = id % ph;
	int y = id / ph;

	//if (x < 5 || x > 507 || y < 5 || y > 507) return;

	//uint w = pw * 32, h = ph;
	if (x < 1 || x > 1646 || y < 1 || y > 1646) return;
	// count active neighbors
	uint n = GetBit( x - 1, y - 1, pw, second ) + GetBit( x, y - 1, pw, second ) + GetBit( x + 1, y - 1, pw, second ) + GetBit( x - 1, y, pw, second ) + 
				GetBit( x + 1, y, pw, second ) + GetBit( x - 1, y + 1, pw, second ) + GetBit( x, y + 1, pw, second ) + GetBit( x + 1, y + 1, pw, second );
	if ((GetBit( x, y, pw, second ) == 1 && n == 2) || n == 3) BitSet( x, y, pw, pattern );
	//TestBitSet( x, y, pw, pattern );
}

__kernel void copy_function( global uint *second, global uint *pattern )
{	
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 54 * idy;
	if (id >= (54 * 1647)) return;
	second[id] = pattern[id];
	pattern[id] = 0;
}

__kernel void show_function( write_only image2d_t a, uint pw, uint xoffset, uint yoffset, global uint *second, global uint *pattern )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;
	if (id >= (512 * 512)) return;

	int x = id % 512;
	int y = id / 512;

	//if (x < 1 || x > 511 || y < 1 || y > 511) return;

	write_imagef( a, (int2)(x, y), GetBit(x + xoffset, y + yoffset, pw, second) );

}

__kernel void clear_function(
	write_only image2d_t a)
{
	// get the thread id; this will be our pixel id
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;
	if (id >= (512 * 512)) return;

	//int id = get_global_id( 0 );
	//if (id >= (512 * 512)) return;

	int x = id % 512;
	int y = id / 512;
	write_imagef( a, (int2)(x, y), 0 );
}
