import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { Segment, Breadcrumb, Image } from "semantic-ui-react";
import "./help.css";

export const BOM = () => {
  const navigate = useNavigate();
  return (
    <div className="help">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>
          Home
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/help")}>
          Help
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>BOM</Breadcrumb.Section>
      </Breadcrumb>
      <h1>Bill of Materials (BOM)</h1>

      <Segment raised>
        <p>
        BOM, or Bill of Materials, allows you to manage all of the parts in your project and give you cost information, status on your inventory, and PCB management. Your BOM tracks the quantities used in your project, allows you to consume those parts as you produce PCB's, determine if you have enough inventory to create the number of PCB's you want to produce, and create PCB serial numbers.
        </p>

        <h3>Getting Started</h3>
        <p>
        The first thing you need to do is create a Project. Click on the `BOM` icon on the home page. 
        </p>
        
        <div className="bullet highlight">
          <p>
          Create your first project by clicking the `+ Add BOM Project` button.
          </p>
          <div className="helpimage large">
            <img src="https://user-images.githubusercontent.com/2531058/231594318-c4248cdb-a1aa-4d5e-a7f5-cbfb7f345b7f.png" alt="Create Project" />
          </div>
        </div>

        <div className="bullet highlight">
          <p>
          Fill in your project details - only a name is required. Adding additional information is purely for your reference. You may associate a BOM with a location, as well as a color which will be displayed on the BOM management page.
          </p>
          <p>
          After adding your project, click on it in the list below to view the BOM interface.
          </p>

          <div className="helpimage large">
            <img src="https://user-images.githubusercontent.com/2531058/231598639-a4d247f9-e2d0-440f-be11-61f841a2ea57.png" alt="BOM Interface" />
          </div>
        </div>
        <p>
        The BOM page allows you to add PCB's and parts. If your project has multiple PCB's, you can create and track parts for each PCB. You can download your BOM to an Excel file, or CSV for when it comes time to order out of stock items or import it into another system.
        </p>
        
        <h4>Important Concepts</h4>

        <ul>
          <li> Multiple PCB's can be created per project, but you can also use no PCB's.</li>
          <li> Parts can be assigned to a PCB, or not assigned to any PCB. These are called Unassociated parts and are still part of your BOM.</li>
          <li> Producing a PCB means you are physically assembling a PCB which will take parts out of your inventory.</li>
        </ul>
        
        <h3 style={{marginTop: '75px'}}>Creating a BOM <i>without</i> PCBs</h3>

        <p>
        If you choose to create your BOM without PCBs, you still have access to most features _except_ for serial number generation and tracking. You can also mix using PCBs and not using PCBs in the same BOM - unassociated parts are essentially treated as a PCB.
        </p>

        <h5>Add a part</h5>

        <p>
        The Add Part dialog is for adding a part to your BOM. Here you can select a PCB (optionally) the part belongs to, the quantity of the part needed for your PCB, and additional designators or notes you may want to provide.
        </p>

        <h5>Adding a part from your inventory</h5>

        <div className="bullet highlight">
          <p>
          Here we have searched our inventory for any part starting with `LM` and have found two items. We want to add an LM358 to our BOM so click the LM358 row to indicate you would like to add this part to your BOM.
          </p>
          <div className="helpimage large">
            <img src="https://user-images.githubusercontent.com/2531058/231600598-21ba8cf0-1386-4f42-9258-56b35f63d068.png" alt="Add Part" />
          </div>
        </div>

        <h5>Adding a part not in your inventory</h5>

        <p>
        You can also add parts you have not entered into your inventory yet. Below we have searched for the part `LM741` and it is not found in your inventory.
        </p>

        <div className="bullet highlight">
          <p>
          Clicking Add on a non-inventory part will display the following dialog:
          </p>
          <div className="helpimage large">
            <img src="https://user-images.githubusercontent.com/2531058/231601255-33ebaef2-42e7-4b6e-a72e-1de05005a1fd.png" alt="Adding non-inventory part" />
          </div>
        </div>

        <p>
        Click OK to add the part to your BOM.
        </p>

        <h5>Producing a PCB</h5>

        <div className="bullet highlight">
          <p>
          Below we can see that we have added 2 parts to our BOM. In the black overview area we are being told we have all parts in stock, and that we can produce our BOM 1 time. In this case, we have not assigned any parts to a PCB so we intend to produce all parts onto a single unspecified PCB. If we have added multiple PCBs to our system this overview will tell you how many times you can produce your BOM before you run out of parts, and which PCB has the lowest number of parts in our inventory.
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231601358-a9164d3d-8c40-4083-bee9-fca08f6e23d7.png" alt="BOM Interface" />
          </div>
        </div>

        <div className="bullet highlight">
          <p>
          Let's try producing a PCB. Click on the `Produce PCB` button.
          </p>
          <p>
          You will be shown the Produce PCB dialog.
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231601398-ec2f381f-1c48-4639-9257-f19a4bf4d7d4.png" alt="Produce PCB" />
          </div>
        </div>

        <p>
        Because we have not added any PCB's, we should choose the All or Unassociated options under Select PCB(S). All means to produce everything in your BOM (all defined PCB's as well as unassociated parts). Unassociated parts in this case has the same meaning, as we only have unassociated parts in our BOM.
        </p>

        <div className="bullet highlight">
          <p>
          Enter the quantity you plan to produce. Here we will produce a quantity of 1 as we do not have enough parts for more than that. 
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231603245-b0525fe8-2bd0-457e-bbf6-308bf74fd696.png" alt="Produce PCB" />
          </div>
        </div>

        <div className="bullet highlight">
          <p>
          No serial numbers will be generated, as that is only an option for when you produce a PCB. Click on the `Produce` button.
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231603152-f737e0b3-78d8-4abe-bbc7-239ead0b48dd.png" alt="BOM Interface" />
          </div>
        </div>

        <p>
        Now we can see we our inventory was reduced by the total number of parts needed to produce our BOM. We have 1 out of stock part - in this case `LM741` and we must purchase more and add them to our inventory.
        </p>

        <h3 style={{marginTop: '75px'}}>Creating a BOM <i>with</i> PCBs</h3>

        <p>
        BOM's are even more useful when you utilize PCBs. They are very easy to use, even if you only have 1 PCB in your project as they can create and track serial number generation for you. You can also add unassociated parts (not assigned to a PCB) should you need to, they will be treated as an extra PCB.
        </p>

        <h4>Create a PCB</h4>

        <div className="bullet highlight">
        <p>
        Click on the `Create PCB` button.
        </p>
        <div className="helpimage large">
        <img src="https://user-images.githubusercontent.com/2531058/231605796-5b67a832-4a87-4c01-bb6d-9895a296b025.png" alt="Create PCB" />
        </div>
        </div>

        <div className="bullet highlight">
        <p>
        Enter a name, here we will use `Power Supply`. Enter an optional description. Additionally you can specify the quantity of the PCB that is required to produce your BOM - normally this is `1`. But in special cases such as an audio amplifier board, you may have 2 of the same PCB's required for your BOM project (left and right channels). The quantity will be treated as a multiplier of the parts assigned to this PCB. You can specify a serial number format of your choice, this value will be incremented from the right most portion of the format you specify. Cost is used for cost projections of your BOM.
        </p>
        <p>
        Repeat this step by also creating a PCB named `Sensor Board` with a quantity of 1.
        </p>
        </div>
        
        <h4>Add a part</h4>

        <p>
        The Add Part dialog is for adding a part to your BOM. Here you can select a PCB the part belongs to, the quantity of the part needed for your PCB, and additional designators or notes you may want to provide. Select the `Power Supply` PCB we just created.
        </p>

        <h5>Adding a part from your inventory</h5>

        <div className="bullet highlight">
          <p>Here we have searched our inventory for any part starting with `LM` and have found two items. We want to add an LM358 to our BOM so click the LM358 row to indicate you would like to add this part to your BOM.</p>

          <div className="helpimage large">
            <img src="https://user-images.githubusercontent.com/2531058/231606379-1e58e30e-74c9-4976-ac08-6b8089721b93.png" alt="Add Part" />
          </div>
        </div>

        <h5>Adding a part not in your inventory</h5>
        
        <div className="bullet highlight">
          <p>
          You can also add parts you have not entered into your inventory yet. Below we have searched for the part `LM741` and it is not found in your inventory.
          </p>

          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231606508-7cc10782-874b-496e-a159-3006a39817ab.png" alt="Add Part" />
          </div>
        </div>

        <div className="bullet highlight">
          <p>
          Clicking Add on a non-inventory part will display the following dialog:
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231606536-eb1f209a-6fdf-45ad-bf9e-6fa83c36ffb3.png" alt="Add Non-Inventory Part" />
          </div>
        </div>
        <p>
        Click OK to add the part to your BOM.
        </p>

        <h5>Producing a PCB</h5>

        <p>
        Below we can see that we have added 2 parts to our BOM in the `All` tab. The `Power Supply` tab should also show 2 parts, and we have not assigned any parts to the `Sensor Board` yet.
        </p>

        <div className="bullet highlight">
          <p>
          In the black overview area we are being told we have all parts in stock, and the Power Supply PCB is lowest on inventory. We are also shown that we can produce our entire BOM 1 time. This overview will tell you how many times you can produce your BOM before you run out of parts, and which PCB has the lowest number of parts in our inventory for your reference.
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231606987-bde8254d-ccda-4c09-9f47-3d13d22dfe67.png" alt="BOM Interface" />
          </div>
        </div>

        <p>
        Let's try producing a PCB. Click on the `Produce PCB` button.
        </p>

        <div className="bullet highlight">
          <p>
          You will be shown the Produce PCB dialog.
          </p>
          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231607028-65c0f273-b46d-4f34-b151-bf43a8337a89.png" alt="Produce PCB" />
          </div>
        </div>

        <p>
        Select the `Power Supply` PCB. We could choose `All` means to produce everything in your BOM (all defined PCB's as well as unassociated parts). An overview will be displayed in the dialog for each PCB you are producing, and if you have enough parts to produce the quantity of boards.
        </p>

        <div className="bullet highlight">
          <p>
          Enter the quantity you plan to produce. Here we will produce a quantity of 1 as we do not have enough parts for more than that. 
          </p>

          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231607257-5e87c30c-5128-4064-8bd1-25647347c8e7.png" alt="Produce PCB" />
          </div>
        </div>

        <div className="bullet highlight">
          <p>
          You are shown the next serial number that will be generated for each PCB you produce. Click on the `Produce` button.
          </p>

          <div className="helpimage large">
          <img src="https://user-images.githubusercontent.com/2531058/231607424-90ee9b00-0dc4-4421-a832-b0a80c085074.png" alt="BOM Interface" />
          </div>
        </div>

        <p>
        Now we can see we our inventory was reduced by the total number of parts needed to produce our BOM. We have 1 out of stock part - in this case `LM741` and we must purchase more and add them to our inventory.
        </p>

        <p className="centered">
          See the <a href="https://github.com/replaysMike/Binner/wiki" target="_blank" rel="noreferrer">Wiki</a> for more help topics.
        </p>
      </Segment>
    </div>
  );
};
