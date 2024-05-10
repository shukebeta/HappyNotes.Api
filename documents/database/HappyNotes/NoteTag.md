# Database: HappyNotes Table: NoteTag

 Field    | Type            | Null | Default | Comment
----------|-----------------|------|---------|---------
 Id       | bigint unsigned | NO   |         |
 NoteId   | bigint unsigned | NO   | 0       |
 TagId    | bigint unsigned | NO   | 0       |
 CreateAt | bigint          | NO   | 0       |

## Indexes: 

 Key_name       | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------------|-------------|--------------|------------|------------|---------
 PRIMARY        | Id          |            1 |          0 | BTREE      | YES
 UI_NoteIdTagId | NoteId      |            1 |          0 | BTREE      | YES
 UI_NoteIdTagId | TagId       |            2 |          0 | BTREE      | YES
 I_TagId        | TagId       |            1 |          1 | BTREE      | YES
 I_NoteId       | NoteId      |            1 |          1 | BTREE      | YES
