namespace CraftedSolutions.MarBasSchema.Access
{
    public class SchemaAccessDeniedException(GrainAccessFlag requestedAcces)
        : UnauthorizedAccessException(FormatMessage(requestedAcces))
    {
        public GrainAccessFlag RequestedAcces { get; set; } = requestedAcces;

        protected static string FormatMessage(GrainAccessFlag requestedAcces)
        {
            var mod = "Access to";
            if (requestedAcces.HasFlag(GrainAccessFlag.Write) || requestedAcces.HasFlag(GrainAccessFlag.WriteTraits))
            {
                mod = "Modifying of";
            }
            else if (requestedAcces.HasFlag(GrainAccessFlag.Delete))
            {
                mod = "Deleting of";
            }
            else if (requestedAcces.HasFlag(GrainAccessFlag.CreateSubelement))
            {
                mod = "Creating objects under";
            }
            return $"{mod} at least one of the elements is prohibited by ACL";
        }
    }
}
