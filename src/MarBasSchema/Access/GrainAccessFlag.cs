namespace MarBasSchema.Access
{
    [Flags]
    public enum GrainAccessFlag: uint
    {
        None = 0x000,
        Read = 0x001,
        Write = 0x002,
        Delete = 0x004,
        ModifyAcl = 0x008,
        CreateSubelement = 0x010,
        WriteTraits = 0x020,
        Publish = 0x100,
        TakeOwnership = 0x200,
        TransferOwnership = 0x400,
        Full = 0xffffffff
    }
}
