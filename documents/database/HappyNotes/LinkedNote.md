# Database: HappyNotes Table: LinkedNote

 Field        | Type   | Null | Default | Comment
--------------|--------|------|---------|---------
 Id           | bigint | NO   |         |
 NoteId       | bigint | NO   |         |
 LinkedNoteId | bigint | NO   |         |
 CreateAt     | bigint | NO   |         |

## Indexes: 

 Key_name     | Column_name  | Seq_in_index | Non_unique | Index_type | Visible
--------------|--------------|--------------|------------|------------|---------
 PRIMARY      | Id           |            1 |          0 | BTREE      | YES
 uniq         | NoteId       |            1 |          0 | BTREE      | YES
 uniq         | LinkedNoteId |            2 |          0 | BTREE      | YES
 linkedNoteId | LinkedNoteId |            1 |          1 | BTREE      | YES
