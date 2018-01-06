__kernel void device_function( write_only image2d_t a )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 960 * idy;
	if (id >= (960 * 600)) return;
	int2 pos = (int2)( idx, idy );
	write_imagef( a, pos, (float4)(idx / 960.0f, idy / 960.0f, 0, 1 ) );
}