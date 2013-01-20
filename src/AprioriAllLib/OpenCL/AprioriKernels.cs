using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCL.Abstract;

namespace AprioriAllLib
{
	public class MultiItemSupportKernel : Kernel
	{
		private Buffer<int> dataBuffer;

		private Buffer<int> lengthBuffer;

		private Buffer<int> zoneLengthBuffer;

		private Buffer<int> zonesStartsBuffer;

		private Buffer<int> zonesCountBuffer;

		private Buffer<int> includedZonesBuffer;

		private Buffer<int> includedZonesCountBuffer;

		private Buffer<int> stepBuffer;

		public MultiItemSupportKernel(Program program, string kernelName)
			: base(program, kernelName)
		{
		}

		public void SetArguments(Buffer<int> b, Buffer<int> bLength,
			Buffer<int> itemsCount, Buffer<int> itemsTimesUniqueSequence, Buffer<int> uniqueItemsCount,
			Buffer<int> candidateSequence, Buffer<int> candidateSequenceLength, Buffer<int> reductionStep)
		{
			this.dataBuffer = b;
			this.lengthBuffer = bLength;
			this.zoneLengthBuffer = itemsCount;
			this.zonesStartsBuffer = itemsTimesUniqueSequence;
			this.zonesCountBuffer = uniqueItemsCount;
			this.includedZonesBuffer = candidateSequence;
			this.includedZonesCountBuffer = candidateSequenceLength;
			this.stepBuffer = reductionStep;

			base.SetArguments(dataBuffer, lengthBuffer, zoneLengthBuffer, zonesStartsBuffer,
				zonesCountBuffer, includedZonesBuffer, includedZonesCountBuffer, stepBuffer);
		}

		/// <summary>
		/// Launches the Kernel on a given command queue.
		/// </summary>
		/// <param name="queue">command queue</param>
		public void Launch(CommandQueue queue)
		{
			Launch2D(queue, (uint)lengthBuffer.Array[0], 1,
				(uint)zonesCountBuffer.Array[0], 1);
		}

	}
}
