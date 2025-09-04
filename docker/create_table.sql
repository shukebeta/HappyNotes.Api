DROP TABLE IF EXISTS NoteIndex;

CREATE TABLE NoteIndex (
    Id BIGINT,
    UserId BIGINT,
    IsLong INT,
    IsPrivate INT,
    IsMarkdown INT,
    CreatedAt BIGINT,
    UpdatedAt BIGINT,
    DeletedAt BIGINT,
    Content TEXT,
    Tags TEXT
)
ngram_len='2'
ngram_chars='U+3400..U+4DBF,U+4E00..U+9FFF,U+F900..U+FAFF'
dict='keywords'
min_word_len='1'
charset_type='utf-8'
enable_star='0';
