namespace MMG_AdminTool.Models
{
    public enum ModalSize
    {
        Sm, Md, Lg, Xl
    }

    public class ModalButton
    {
        public string Text { get; set; } = "확인";
        public string CssClass { get; set; } = "btn btn-primary";
        public string Id { get; set; } = "";
        public bool DismissOnClick { get; set; } = false; // true면 클릭 시 모달 닫힘
    }

    public class ModalOptions
    {
        public string Id { get; set; } = "app-modal"; // 페이지 내 유일 ID
        public string Title { get; set; } = "";
        public ModalSize Size { get; set; } = ModalSize.Md;

        public string BodyHtml { get; set; } = "";
        public string? BodyPartialView { get; set; }
        public object? BodyModel { get; set; }

        public List<ModalButton> Buttons { get; set; } = new()
        {
            new ModalButton { Text = "닫기", CssClass = "btn btn-secondary", DismissOnClick = true }
        };
    }
}
