ALTER TABLE HappyNotes.Note ADD IsMarkdown TINYINT NOT NULL DEFAULT '0' COMMENT 'indicate content field is in markdown format or not' AFTER IsPrivate;
