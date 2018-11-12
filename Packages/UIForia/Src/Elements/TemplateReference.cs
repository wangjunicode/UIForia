public struct TemplateReference {

    public readonly ushort templateId;
    public readonly ushort memberId;

    public TemplateReference(ushort templateId, ushort memberId) {
        this.templateId = templateId;
        this.memberId = memberId;
    }

}