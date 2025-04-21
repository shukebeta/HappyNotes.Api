# Database: HappyNotes Table: LastSynced

 Field       | Type      | Null | Default           | Comment
-------------|-----------|------|-------------------|---------
 Id          | int       | NO   |                   |
 LastNoteId  | bigint    | NO   |                   |
 LastUpdated | timestamp | YES  | CURRENT_TIMESTAMP |

## Indexes: 

 Key_name | Column_name | Seq_in_index | Non_unique | Index_type | Visible
----------|-------------|--------------|------------|------------|---------
 PRIMARY  | Id          |            1 |          0 | BTREE      | YES
