use CityInfoDB
go

SELECT * FROM Cities
SELECT * FROM PointsOfInterest

SELECT * FROM Cities c inner join PointsOfInterest p on c.id = p.CityId

delete FROM Cities
delete FROM PointsOfInterest
