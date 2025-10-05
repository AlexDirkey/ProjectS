namespace DataAccess.Entities
{
    public partial class Spellclassjunction
    {
        public string Spellid { get; set; } = null!;
        public string Classid { get; set; } = null!;

        public virtual Spell? Spell { get; set; }
        public virtual Class? Class { get; set; }
    }
}