/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package amla;

import javafx.application.Application;
import javafx.geometry.Pos;
import javafx.scene.Scene;
import javafx.scene.layout.HBox;
import javafx.scene.text.Text;
import javafx.stage.Stage;

/**
 *
 * @author Kees Kint
 */
public class AMLA extends Application {
    Stage passStage;
    
    @Override
    public void start(Stage stage) throws Exception {
        /*Parent root = FXMLLoader.load(getClass().getResource("HomePage.fxml"));
        
        Scene scene = new Scene(root);
    
        stage.setScene(scene);
        stage.show();*/
        
        HBox root = new HBox();
        root.setPrefSize(600, 400);
        root.setAlignment(Pos.CENTER);
        Text message = new Text("Failure!!");
        root.getChildren().add(message);
        
        Scene scene = new Scene(root);
        Switchable.setScene(scene);
        Switchable.switchTo("HomePage");
        
        stage.setScene(scene);
        passStage = stage;
        stage.show();
    }

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        launch(args);
    }
    
}
