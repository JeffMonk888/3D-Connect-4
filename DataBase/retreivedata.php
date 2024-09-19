<?php
    $con = mysqli_connect('localhost', 'root', 'root', '3D_Connect_4');

    if (mysqli_connect_errno()) {
        echo "1: Connection Failed"; // error code #1 = connection failed
        exit();
    }

    $userId = mysqli_real_escape_string($con, $_POST["userid"]);

    $queryGetGames = "
        SELECT 
            gi.SessionID,
            opp.username AS OpponentUsername,
            (1000 + IFNULL((SELECT SUM(t.PointChange) FROM Transaction t WHERE t.UserID = opp.id), 0)) AS OpponentScore,
            (SELECT username FROM UserID WHERE id = gi.WinnerID) AS WinnerUsername,
            CASE 
                WHEN gu.Colour = 1 THEN 'Black'
                WHEN gu.Colour = 2 THEN 'White'
            END AS UserRole,
            JSON_LENGTH(gi.Moves) AS TotalMoves
        FROM 
            Game_UserID gu
        INNER JOIN Session gi ON gu.SessionID = gi.SessionID
        INNER JOIN UserID usr ON gu.UserID = usr.id
        INNER JOIN UserID opp ON opp.id <> usr.id AND opp.id IN 
            (SELECT UserID FROM Game_UserID WHERE SessionID = gi.SessionID AND UserID <> gu.UserID)
        WHERE 
            gu.UserID = $userId
        ORDER BY 
            gi.SessionID DESC
        LIMIT 10;
    ";


    $resultGetGames = mysqli_query($con, $queryGetGames);

    if (!$resultGetGames) {
        echo "2: Game info retrieval query failed - " . mysqli_error($con);
        exit();
    }

    if (mysqli_num_rows($resultGetGames) > 0) {
        $games = mysqli_fetch_all($resultGetGames, MYSQLI_ASSOC);
        echo json_encode($games);
    } else {
        echo "3: No games found for the specified user.";
    }
    

    mysqli_close($con);
?>
