# Database: HappyNotes Table: NoteTag

 Field     | Type            | Null | Default | Comment
-----------|-----------------|------|---------|-----------------------
 Id        | bigint unsigned | NO   |         |
 UserId    | bigint          | NO   |         | =Note.UserId
 NoteId    | bigint unsigned | NO   |         |
 Tag       | varchar(32)     | NO   |         | Note tag in lowercase
 CreatedAt | bigint          | NO   | 0       | A unix timestamp

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 NoteId      | NoteId      |            1 |          0 | BTREE      | YES
 NoteId      | Tag         |            2 |          0 | BTREE      | YES
 idx_TagName | Tag         |            1 |          1 | BTREE      | YES
