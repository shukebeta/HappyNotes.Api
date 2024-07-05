# Database: HappyNotes Table: NoteTag

 Field    | Type            | Null | Default | Comment
----------|-----------------|------|---------|---------
 Id       | bigint unsigned | NO   |         |
 NoteId   | bigint unsigned | NO   |         |
 TagName  | varchar(128)    | NO   |         |
 CreateAt | bigint          | NO   |         |

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 NoteId      | NoteId      |            1 |          0 | BTREE      | YES
 NoteId      | TagName     |            2 |          0 | BTREE      | YES
 idx_TagName | TagName     |            1 |          1 | BTREE      | YES
