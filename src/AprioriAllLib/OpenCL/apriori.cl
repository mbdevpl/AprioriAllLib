/// \addtogroup opencl
/// @{

//bool rangeIsValid(const int valuesCount, const int includedValuesCount, 
//	const int step, const int threadId, const int index)
//{
//	if(index < valuesCount && threadId < includedValuesCount
//		&& index + step < valuesCount && threadId + step < includedValuesCount
//		&& step > 0 && threadId % (2 * step) == 0)
//		return true;
//	else
//		return false;
//}

/*!
	multiItemSupport
*/
__kernel void multiItemSupport(
	__global int* values, // supports
	__global const int* valuesCount, // supportsCount
	__global const int* zoneLength, // itemsCount
	__global const int* zonesStarts, // sequence of products used by 2nd dim
	__global const int* zonesCount, // currently NOT USED // uniqueItemsCount
	__global const int* includedZones, // current candidate set
	__global const int* includedZonesCount, // length of current candidate set
	__global const int* step // reduction step
	)
{
	int valueId = get_global_id(0);
	int zoneId = get_global_id(1);

	if(valueId < *zoneLength && zoneId < *includedZonesCount
		&& zoneId + *step < *includedZonesCount
		&& *step > 0 && valueId % (2 * *step) == 0)
	{
		int index = valueId + zonesStarts[ includedZones[zoneId] ];
		int indexNext = valueId + zonesStarts[ includedZones[zoneId + *step] ];

		if(index < *valuesCount && indexNext < *valuesCount)
		{
			if(values[index] == 0 && values[indexNext] != 0)
				values[index] = values[indexNext];
			values[indexNext] = 0;
		}
	}
}

/// @}
