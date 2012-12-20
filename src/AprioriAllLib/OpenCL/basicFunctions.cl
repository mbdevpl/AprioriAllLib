/// \addtogroup opencl
/// @{

__kernel void assignZero(
	__global int* values,
	__global const int* valuesCount
	)
{
	int x = get_global_id(0);
	if(x < *valuesCount)
	{
		values[x] = 0;
	}
}

__kernel void assignSingleValue(
	__global int* values,
	__global const int* valuesCount,
	__global const int* assignedValue
	)
{
	int x = get_global_id(0);
	if(x < *valuesCount)
	{
		values[x] = *assignedValue;
	}
}

/*!
	Executes pairwise 'or' functions on two given arrays, 
	and stores the result in the second array.
 */
__kernel void logicOr(
	__global const int* sourceValues,
	__global int* resultingValues,
	__global const int* valuesCount
	)
{
	int x = get_global_id(0);
	if(x < *valuesCount)
	{
		if(sourceValues[x] == 1)
			resultingValues[x] = 1;
	}
}

/// @}
