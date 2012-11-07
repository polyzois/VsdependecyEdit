namespace ui.Controllers
{
    public class ProjRef
    {
        public string ProjectName { get; set; }
        public string Ref { get; set; }
        public string OldRef { get; set; }

        public override string ToString()
        {
            return string.Format("ProjectName: {0}, Ref: {1}, OldRef: {2}", ProjectName, Ref, OldRef);
        }
    }
}