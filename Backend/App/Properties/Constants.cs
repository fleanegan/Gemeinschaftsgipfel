namespace Gemeinschaftsgipfel.Properties;

public static class Constants
{
    public const int MaxLengthTitle = 150;
    public const string MaxLengthTitleErrorMessage = "Vortragstitel dürfen maximal {0} Zeichen lang sein.";
    public const string EmptyTitleErrorMessage = "Ein Vortrag braucht einen Titel";

    public const string WrongPassphraseErrorMessage =
        "Falsches Eintrittsgeheimnis. Stell sicher, dass du den richtigen Satz aus der Einladungsnachricht kopierst";

    public const int MaxLengthDescription = 10000;

    public const string MaxLengthDescriptionErrorMessage =
        "Vortragsbeschreibungen dürfen maximal 10000 Zeichen lang sein.";

    public const string EmptyIdErrorMessage =
        "Die Vortragsidentitifikationsnummer ist obligatorisch, um Vortragsthemen zu modifizieren.";

    public const string MissingPresentationTimeErrorMessage = "Gib an, wie lang der Vortrag geht.";

    public const int MaxLengthTopicCommentContent = 5000;

    public const int MaxLengthMaterial = 500;
    public const string MaxLengthMaterialErrorMessage = "Die Materialbeschreibung darf maximal {0} Zeichen lang sein.";
    public const string MissingCategoryErrorMessage = "Bitte wähle eine Kategorie für deinen Vortrag.";

    public const int MaxLengthForumEntryTitle = 150;
    public const string MaxLengthForumEntryTitleErrorMessage = "Forumsbeitragstitel dürfen maximal {0} Zeichen lang sein.";
    public const string EmptyForumEntryTitleErrorMessage = "Ein Forumsbeitrag braucht einen Titel";

    public const int MaxLengthForumEntryContent = 10000;
    public const string MaxLengthForumEntryContentErrorMessage = "Forumsbeitragsinhalt darf maximal {0} Zeichen lang sein.";
    public const string EmptyForumEntryContentErrorMessage = "Ein Forumsbeitrag braucht Inhalt";

    public const int MaxLengthForumCommentContent = 5000;
}
