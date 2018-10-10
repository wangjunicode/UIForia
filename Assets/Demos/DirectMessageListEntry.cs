namespace Src {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Style classPath='DirectMessageListEntry+Style'/>
        <Contents>
    
            <Image src='{url(chatData.iconUrl)}'/>
            <Text>Name Here</Text>
            <Group>
                <Group style='activity-indicator'/>
            </Group>
        </Contents>
    </UITemplate>
    ")]
    public class DirectMessageListEntry : UIElement {

        public bool isSelected;
        public ChatData chatData;

    }

}