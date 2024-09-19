<?php
    $con = mysqli_connect('localhost', 'root', 'root', '3D_Connect_4');

    if (mysqli_connect_errno()) {
        echo "1: Connection failed"; // error code for connection failure
        exit();
    }

    if (!isset($_POST["SessionID"])) {
        echo "3: SessionID not provided"; // error code for missing SessionID
        exit();
    }

    $SessionID = $_POST["SessionID"]; // Missing semicolon added here

    // Prepare the statement to select the Moves from GameInfo table for the given SessionID
    $stmt = $con->prepare("SELECT Moves FROM Session WHERE SessionID = ?");
    if ($stmt === false) {
        echo "4: Query preparation failed"; // error code for preparation failure
        exit();
    }

    $stmt->bind_param("i", $SessionID); // Corrected typo here
    $stmt->execute();
    $result = $stmt->get_result();

    if ($row = $result->fetch_assoc()) {
        // Directly output the moves as the response
        echo "0"."\t" . $row['Moves']; // 0 indicates success followed by the moves data
    } else {
        echo "2: No moves are found for the sessionID"; // error code for no moves found
    }

    $stmt->close();
    $con->close();
?>
