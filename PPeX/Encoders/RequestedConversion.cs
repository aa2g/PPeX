namespace PPeX.Encoders
{
	public class RequestedConversion
	{
		public ArchiveFileType TargetEncoding { get; }

		public object EncodingParameters { get; }

		internal RequestedConversion(ArchiveFileType targetEncoding, object encodingParameters)
		{
			TargetEncoding = targetEncoding;
			EncodingParameters = encodingParameters;
		}
	}
}