DROP TABLE IF EXISTS ChinaProvinceCity;
ALTER TABLE province RENAME TO ChinaProvinceCity;
ALTER TABLE ChinaProvinceCity CHANGE id Id int unsigned NOT NULL AUTO_INCREMENT comment"主键，自增Id";
ALTER TABLE ChinaProvinceCity CHANGE code Code bigint NOT NULL COMMENT '行政区划码';
ALTER TABLE ChinaProvinceCity CHANGE name Name varchar(32) DEFAULT NULL COMMENT '名称';
ALTER TABLE ChinaProvinceCity CHANGE province Province varchar(32) DEFAULT NULL COMMENT '省/直辖市';
ALTER TABLE ChinaProvinceCity CHANGE city City varchar(32) DEFAULT NULL COMMENT '市';
ALTER TABLE ChinaProvinceCity CHANGE area Area varchar(32) DEFAULT NULL COMMENT '区';
ALTER TABLE ChinaProvinceCity CHANGE town Town varchar(32) DEFAULT NULL COMMENT ' 城镇地区';

