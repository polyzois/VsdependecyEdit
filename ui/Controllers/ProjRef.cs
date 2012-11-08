namespace ui.Controllers
{
    public class ProjRef
    {
        public string ProjectName { get; set; }
        public string Ref { get; set; }
        public string NewRef { get; set; }

        public bool FileExists { get; set; }

        public override string ToString()
        {
            return string.Format("ProjectName: {0}, Ref: {1}, NewRef: {2}", ProjectName, Ref, NewRef);
        }
    }
}