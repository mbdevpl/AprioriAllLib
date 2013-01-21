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

	\param step reduction step number
*/
__kernel void multiItemSupport(
	__global int* values, // transactionsSupports
	__global const int* valuesCount, // transactionsSupportsCount
	__global const int* transactionsCount,
	//__global const int* uniqueItems,
	//__global const int* uniqueItemsCount,
	__global const int* candidate, // current candidate set
	__global const int* candidateCount, // length of current candidate set
	__global const int* step // 
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);

	if(*step > 0 && y % (2 * *step) == 0
		&& x < *transactionsCount && y < *candidateCount && y + *step < *candidateCount)
	{
		int transactionId = x;
		int uniqueItemId = candidate[y];
		int uniqueItemIdNext = candidate[y + *step];

		int index = transactionId + *transactionsCount * uniqueItemId;
		int indexNext = transactionId + *transactionsCount * uniqueItemIdNext;

		if(index < *valuesCount && indexNext < *valuesCount
			&& index < *transactionsCount * (uniqueItemId + 1)
			&& indexNext < *transactionsCount * (uniqueItemIdNext + 1))
		{
			if(values[index] == 0 || values[indexNext] == 0)
				values[index] = 0;
			values[indexNext] = 0;
		}
	}
}

/// @}
