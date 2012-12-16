/// \addtogroup opencl
/// @{

/*!
	Counts distinct item values in a list, and creates an ID
	for each unique value.
 */
__kernel void findDistinctItems(
		__global const int* itemList,
		__global const int* itemListLength, 
		__global const int* itemListLimits, 
		__global const int* itemLitsLimitsCount, 
		__global int* ids
		)
{


}

/// @}
