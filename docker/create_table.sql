CREATE TABLE IF NOT EXISTS
    NoteIndex (
        Id BIGINT,
        UserId BIGINT,
        IsLong INT,
        IsPrivate INT,
        IsMarkdown INT,
        CreatedAt BIGINT,
        UpdatedAt BIGINT,
        DeletedAt BIGINT,
        Content TEXT,
        Tags STRING
    ) morphology='stem_en,icu_chinese' charset_table='non_cjk,chinese';
