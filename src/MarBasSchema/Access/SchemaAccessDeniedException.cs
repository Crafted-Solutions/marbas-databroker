namespace MarBasSchema.Access
{
    public class SchemaAccessDeniedException: UnauthorizedAccessException
    {
        public SchemaAccessDeniedException(GrainAccessFlag requestedAcces)
            : base(FormatMessage(requestedAcces))
            => RequestedAcces = requestedAcces;

        public GrainAccessFlag RequestedAcces { get; set; }

        protected static string FormatMessage(GrainAccessFlag requestedAcces)
        {
            var mod = "Access to";
            if (GrainAccessFlag.Write == (GrainAccessFlag.Write & requestedAcces) || GrainAccessFlag.WriteTraits == (GrainAccessFlag.WriteTraits & requestedAcces))
            {
                mod = "Modifying of";
            }
            else if (GrainAccessFlag.Delete == (GrainAccessFlag.Delete & requestedAcces))
            {
                mod = "Deleting of";
            }
            else if (GrainAccessFlag.CreateSubelement == (GrainAccessFlag.CreateSubelement & requestedAcces))
            {
                mod = "Creating objects under";
            }
            return $"{mod} at least one of the elements is prohibited by ACL";
        }
    }
}
