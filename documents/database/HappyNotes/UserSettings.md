# Database: HappyNotes Table: UserSettings

 Field        | Type          | Null | Default | Comment
--------------|---------------|------|---------|---------
 Id           | bigint        | NO   |         |
 UserId       | bigint        | YES  |         |
 SettingName  | varchar(255)  | YES  |         |
 SettingValue | varchar(4096) | YES  |         |
 CreateAt     | bigint        | NO   |         |
 UpdateAt     | bigint        | YES  |         |
 DeleteAt     | bigint        | YES  |         |

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 SettingName | UserId      |            1 |          0 | BTREE      | YES
 SettingName | SettingName |            2 |          0 | BTREE      | YES
 UserId      | UserId      |            1 |          1 | BTREE      | YES
